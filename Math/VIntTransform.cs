using UnityEngine;

namespace MobaGame
{
    namespace FixedMath
    {
        public struct VIntTransform
        {
            public VInt3 position;
            VInt3 _up;
            VInt3 _right;
            VInt3 _forward;

            public static readonly VIntTransform Identity = new VIntTransform(VInt3.zero, VInt3.right, VInt3.up, VInt3.forward);

            public VIntTransform(Transform trans)
            {
                position = new VInt3(trans.position);
                _up = new VInt3(trans.up);
                _right = new VInt3(trans.right);
                _forward = new VInt3(trans.forward);
            }

            public VIntTransform(VInt3 position, VInt3 right, VInt3 up, VInt3 forward)
            {
                this.position = position;
                _up = up;
                _right = right;
                _forward = forward;
            }

            public VInt3 forward
            {
                get
                {
                    return _forward;
                }

                set
                {
                    _forward = value.Normalize();
                }
            }

            public VInt3 up
            {
                get
                {
                    return _up;
                }
            }

            public VInt3 right
            {
                get
                {
                    return _right;
                }
            }

            public VInt3 TransformDirection(VInt3 local)
            {
                return _right * local.x + _up * local.y + _forward * local.z;
            }

            public VInt3 TransformPoint(VInt3 local)
            {
                return _right * local.x + _up * local.y + _forward * local.z + position;
            }

            public VInt3 InverseTransformDirection(VInt3 global)
            {
                return new VInt3(VInt3.Dot(global, right), VInt3.Dot(global, up), VInt3.Dot(global, forward));
            }

            public VInt3 InverseTransformPoint(VInt3 global)
            {
                VInt3 diff = global - position;
                return new VInt3(VInt3.Dot(diff, right), VInt3.Dot(diff, up), VInt3.Dot(diff, forward));
            }

            public VInt3[] getBasis()
            {
                return new VInt3[] { right, up, forward };
            }

            public VInt3[] getTransposeBasis()
            {
                return new VInt3[]
                {
                    new VInt3(right.x, up.x, forward.x),
                    new VInt3(right.y, up.y, forward.y),
                    new VInt3(right.z, up.z, forward.z)
                };
            }

            public VIntTransform Transform(VIntTransform trans)
            {
                VInt3 position = InverseTransformPoint(trans.position);
                VInt3 right = InverseTransformDirection(trans.right);
                VInt3 up = InverseTransformDirection(trans.up);
                VInt3 forward = InverseTransformDirection(trans.forward);
                VIntTransform result = new VIntTransform(position, right, up, forward);
                return result;
            }
        }
    }
}
