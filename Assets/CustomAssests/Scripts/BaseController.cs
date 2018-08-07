#define DEBUG_CC2D_RAYS
using UnityEngine;
using System.Collections.Generic;


public class BaseController : MonoBehaviour
{
    #region internal types

    struct CharacterRaycastOrigins
    {
        public Vector3 topLeft;
        public Vector3 bottomRight;
        public Vector3 bottomLeft;
    }

    public class CharacterCollisionState2D
    {
        public bool right;
        public bool left;
        public bool above;
        public bool below;
        public bool becameGroundedThisFrame;
        public bool wasGroundedLastFrame;
        public bool movingDownSlope;
        public float slopeAngle;


        public bool hasCollision()
        {
            return below || right || left || above;
        }

        public void reset()
        {
            right = left = above = below = becameGroundedThisFrame = movingDownSlope = false;
            slopeAngle = 0f;
        }
    }

    #endregion

    #region events, properties and fields

    [Range(0.001f, 0.3f)]
    private float _skinWidth = 0.02f;

    private float skinWidth
    {
        get { return _skinWidth; }
        set
        {
            _skinWidth = value;
            recalculateDistanceBetweenRays();
        }
    }

    public LayerMask platformMask = 0;

    [Range(0f, 90f)]
    private float slopeLimit = 60f;
    private float jumpingThreshold = 0.07f;


    private AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));

    [Range(2, 20)]
    private int totalHorizontalRays = 8;
    [Range(2, 20)]
    private int totalVerticalRays = 4;

    private float _slopeLimitTangent = Mathf.Tan(75f * Mathf.Deg2Rad);

    private new Transform transform;

    private BoxCollider2D boxCollider;


    private CharacterCollisionState2D collisionState = new CharacterCollisionState2D();

    public Vector3 velocity;
    public bool isGrounded { get { return collisionState.below; } }

    private const float kSkinWidthFloatFudgeFactor = 0.001f;

    #endregion


    private CharacterRaycastOrigins _raycastOrigins;
    private RaycastHit2D _raycastHit;

    private List<RaycastHit2D> _raycastHitsThisFrame = new List<RaycastHit2D>(2);

    private float _verticalDistanceBetweenRays;
    private float _horizontalDistanceBetweenRays;

    private bool _isGoingUpSlope = false;

    #region Monobehaviour

    void Awake()
    {
        transform = GetComponent<Transform>();
        boxCollider = GetComponent<BoxCollider2D>();

        skinWidth = _skinWidth;
    }

    #endregion

    [System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
    void DrawRay(Vector3 start, Vector3 dir, Color color)
    {
        Debug.DrawRay(start, dir, color);
    }


    #region Public

    public void move(Vector3 deltaMovement)
    {
        // save off our current grounded state which we will use for wasGroundedLastFrame and becameGroundedThisFrame
        collisionState.wasGroundedLastFrame = collisionState.below;

        // clear our state
        collisionState.reset();
        _raycastHitsThisFrame.Clear();
        _isGoingUpSlope = false;

        primeRaycastOrigins();


        // first, we check for a slope below us before moving
        // only check slopes if we are going down and grounded
        if (deltaMovement.y < 0f && collisionState.wasGroundedLastFrame)
            handleVerticalSlope(ref deltaMovement);

        // now we check movement in the horizontal dir
        if (deltaMovement.x != 0f)
            moveHorizontally(ref deltaMovement);

        // next, check movement in the vertical dir
        if (deltaMovement.y != 0f)
            moveVertically(ref deltaMovement);

        // move then update our state
        deltaMovement.z = 0;
        transform.Translate(deltaMovement, Space.World);

        // only calculate velocity if we have a non-zero deltaTime
        if (Time.deltaTime > 0f)
            velocity = deltaMovement / Time.deltaTime;

        // set our becameGrounded state based on the previous and current collision state
        if (!collisionState.wasGroundedLastFrame && collisionState.below)
            collisionState.becameGroundedThisFrame = true;

        // if we are going up a slope we artificially set a y velocity so we need to zero it out here
        if (_isGoingUpSlope)
            velocity.y = 0;
    }

    public void recalculateDistanceBetweenRays()
    {
        // figure out the distance between our rays in both directions
        // horizontal
        var colliderUseableHeight = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (2f * _skinWidth);
        _verticalDistanceBetweenRays = colliderUseableHeight / (totalHorizontalRays - 1);

        // vertical
        var colliderUseableWidth = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (2f * _skinWidth);
        _horizontalDistanceBetweenRays = colliderUseableWidth / (totalVerticalRays - 1);
    }

    #endregion


    #region Movement Methods

    void primeRaycastOrigins()
    {
        var modifiedBounds = boxCollider.bounds;
        modifiedBounds.Expand(-2f * _skinWidth);

        _raycastOrigins.topLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
        _raycastOrigins.bottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
        _raycastOrigins.bottomLeft = modifiedBounds.min;
    }


    void moveHorizontally(ref Vector3 deltaMovement)
    {
        var isGoingRight = deltaMovement.x > 0;
        var rayDistance = Mathf.Abs(deltaMovement.x) + _skinWidth;
        var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
        var initialRayOrigin = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;

        for (var i = 0; i < totalHorizontalRays; i++)
        {
            var ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _verticalDistanceBetweenRays);

            DrawRay(ray, rayDirection * rayDistance, Color.red);

            // if we are grounded we will include oneWayPlatforms only on the first ray (the bottom one). this will allow us to
            // walk up sloped oneWayPlatforms
            if (i == 0 && collisionState.wasGroundedLastFrame)
                _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);
            else
                _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, platformMask);

            if (_raycastHit)
            {
                // the bottom ray can hit a slope but no other ray can so we have special handling for these cases
                if (i == 0 && handleHorizontalSlope(ref deltaMovement, Vector2.Angle(_raycastHit.normal, Vector2.up)))
                {
                    _raycastHitsThisFrame.Add(_raycastHit);
                    break;
                }

                // set our new deltaMovement and recalculate the rayDistance taking it into account
                deltaMovement.x = _raycastHit.point.x - ray.x;
                rayDistance = Mathf.Abs(deltaMovement.x);

                // remember to remove the skinWidth from our deltaMovement
                if (isGoingRight)
                {
                    deltaMovement.x -= _skinWidth;
                    collisionState.right = true;
                }
                else
                {
                    deltaMovement.x += _skinWidth;
                    collisionState.left = true;
                }

                _raycastHitsThisFrame.Add(_raycastHit);

                // we add a small fudge factor for the float operations here. if our rayDistance is smaller
                // than the width + fudge bail out because we have a direct impact
                if (rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
                    break;
            }
        }
    }

    bool handleHorizontalSlope(ref Vector3 deltaMovement, float angle)
    {
        // disregard 90 degree angles (walls)
        if (Mathf.RoundToInt(angle) == 90)
            return false;

        // if we can walk on slopes and our angle is small enough we need to move up
        if (angle < slopeLimit)
        {
            // we only need to adjust the deltaMovement if we are not jumping
            // TODO: this uses a magic number which isn't ideal! The alternative is to have the user pass in if there is a jump this frame
            if (deltaMovement.y < jumpingThreshold)
            {
                // apply the slopeModifier to slow our movement up the slope
                var slopeModifier = slopeSpeedMultiplier.Evaluate(angle);
                deltaMovement.x *= slopeModifier;

                // we dont set collisions on the sides for this since a slope is not technically a side collision.
                // smooth y movement when we climb. we make the y movement equivalent to the actual y location that corresponds
                // to our new x location using our good friend Pythagoras
                deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * deltaMovement.x);
                var isGoingRight = deltaMovement.x > 0;

                // safety check. we fire a ray in the direction of movement just in case the diagonal we calculated above ends up
                // going through a wall. if the ray hits, we back off the horizontal movement to stay in bounds.
                var ray = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
                RaycastHit2D raycastHit;
                if (collisionState.wasGroundedLastFrame)
                    raycastHit = Physics2D.Raycast(ray, deltaMovement.normalized, deltaMovement.magnitude, platformMask);
                else
                    raycastHit = Physics2D.Raycast(ray, deltaMovement.normalized, deltaMovement.magnitude, platformMask);

                if (raycastHit)
                {
                    // we crossed an edge when using Pythagoras calculation, so we set the actual delta movement to the ray hit location
                    deltaMovement = (Vector3)raycastHit.point - ray;
                    if (isGoingRight)
                        deltaMovement.x -= _skinWidth;
                    else
                        deltaMovement.x += _skinWidth;
                }

                _isGoingUpSlope = true;
                collisionState.below = true;
            }
        }
        else // too steep. get out of here
        {
            deltaMovement.x = 0;
        }

        return true;
    }


    void moveVertically(ref Vector3 deltaMovement)
    {
        var isGoingUp = deltaMovement.y > 0;
        var rayDistance = Mathf.Abs(deltaMovement.y) + _skinWidth;
        var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
        var initialRayOrigin = isGoingUp ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft;

        // apply our horizontal deltaMovement here so that we do our raycast from the actual position we would be in if we had moved
        initialRayOrigin.x += deltaMovement.x;

        // if we are moving up, we should ignore the layers in oneWayPlatformMask
        var mask = platformMask;

        for (var i = 0; i < totalVerticalRays; i++)
        {
            var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);

            DrawRay(ray, rayDirection * rayDistance, Color.red);
            _raycastHit = Physics2D.Raycast(ray, rayDirection, rayDistance, mask);
            if (_raycastHit)
            {
                // set our new deltaMovement and recalculate the rayDistance taking it into account
                deltaMovement.y = _raycastHit.point.y - ray.y;
                rayDistance = Mathf.Abs(deltaMovement.y);

                // remember to remove the skinWidth from our deltaMovement
                if (isGoingUp)
                {
                    deltaMovement.y -= _skinWidth;
                    collisionState.above = true;
                }
                else
                {
                    deltaMovement.y += _skinWidth;
                    collisionState.below = true;
                }

                _raycastHitsThisFrame.Add(_raycastHit);

                // this is a hack to deal with the top of slopes. if we walk up a slope and reach the apex we can get in a situation
                // where our ray gets a hit that is less then skinWidth causing us to be ungrounded the next frame due to residual velocity.
                if (!isGoingUp && deltaMovement.y > 0.00001f)
                    _isGoingUpSlope = true;

                // we add a small fudge factor for the float operations here. if our rayDistance is smaller
                // than the width + fudge bail out because we have a direct impact
                if (rayDistance < _skinWidth + kSkinWidthFloatFudgeFactor)
                    break;
            }
        }
    }

    private void handleVerticalSlope(ref Vector3 deltaMovement)
    {
        // slope check from the center of our collider
        var centerOfCollider = (_raycastOrigins.bottomLeft.x + _raycastOrigins.bottomRight.x) * 0.5f;
        var rayDirection = -Vector2.up;

        // the ray distance is based on our slopeLimit
        var slopeCheckRayDistance = _slopeLimitTangent * (_raycastOrigins.bottomRight.x - centerOfCollider);

        var slopeRay = new Vector2(centerOfCollider, _raycastOrigins.bottomLeft.y);
        DrawRay(slopeRay, rayDirection * slopeCheckRayDistance, Color.yellow);
        _raycastHit = Physics2D.Raycast(slopeRay, rayDirection, slopeCheckRayDistance, platformMask);
        if (_raycastHit)
        {
            // bail out if we have no slope
            var angle = Vector2.Angle(_raycastHit.normal, Vector2.up);
            if (angle == 0)
                return;

            // we are moving down the slope if our normal and movement direction are in the same x direction
            var isMovingDownSlope = Mathf.Sign(_raycastHit.normal.x) == Mathf.Sign(deltaMovement.x);
            if (isMovingDownSlope)
            {
                // going down we want to speed up in most cases so the slopeSpeedMultiplier curve should be > 1 for negative angles
                var slopeModifier = slopeSpeedMultiplier.Evaluate(-angle);
                // we add the extra downward movement here to ensure we "stick" to the surface below
                deltaMovement.y += _raycastHit.point.y - slopeRay.y - skinWidth;
                deltaMovement.x *= slopeModifier;
                collisionState.movingDownSlope = true;
                collisionState.slopeAngle = angle;
            }
        }
    }

    #endregion
}