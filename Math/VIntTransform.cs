using UnityEngine;

namespace MobaGame
{
    namespace FixedMath
    {
        public struct VIntTransform
        {
            public VInt3 position;
            VIntQuaternion _rotation;
            VInt3 _up;
            VInt3 _right;
            VInt3 _forward;
            VInt3 _localScale;

            public static readonly VIntTransform Identity = new VIntTransform(VInt3.zero, VIntQuaternion.identity);

            public VIntTransform(Transform trans)
            {
                position = new VInt3(trans.position);
                _up = new VInt3(trans.up);
                _right = new VInt3(trans.right);
                _forward = new VInt3(trans.forward);
                _localScale = new VInt3(trans.localScale);
                _rotation = new VIntQuaternion(trans.rotation);
            }

            public VIntTransform(VInt3 position, VIntQuaternion rotation)
            {
                this.position = position;
                _rotation = rotation;
                _up = rotation * VInt3.up;
                _right = rotation * VInt3.right;
                _forward = rotation * VInt3.forward;
                _localScale = VInt3.one;
            }

            public VIntQuaternion rotation
            {
                get
                {
                    return _rotation;
                }

                set
                {
                    _rotation = value;
                    _up = _rotation * VInt3.up;
                    _right = _rotation * VInt3.right;
                    _forward = _rotation * VInt3.forward;
                }
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
                    rotation = VIntQuaternion.FromToRotation(VInt3.forward, value);
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

            public VInt3 scale
            {
                get
                {
                    return _localScale;
                }

                set
                {
                    _localScale = value;
                }
            }

            public VInt3 TransformDirection(VInt3 local)
            {
                return _right * local.x + _up * local.y + _forward * local.z;
            }

            public VInt3 TransformPoint(VInt3 local)
            {
                return _right * _localScale.x * local.x + _up * _localScale.y * local.y + _forward * _localScale.z * local.z + position;
            }

            public VInt3 TransformVector(VInt3 local)
            {
                return _right * _localScale.x * local.x + _up * _localScale.y * local.y + _forward * _localScale.z * local.z;
            }

            public VInt3 InverseTransformDirection(VInt3 global)
            {
                return new VInt3(VInt3.Dot(global, right), VInt3.Dot(global, up), VInt3.Dot(global, forward));
            }

            public VInt3 InverseTransformPoint(VInt3 global)
            {
                return new VInt3(VInt3.Dot(global - position, right) / _localScale.x, VInt3.Dot(global - position, up) / _localScale.y, VInt3.Dot(global - position, forward) / _localScale.z);
            }

            public VInt3 InverseTransformVector(VInt3 global)
            {
                return new VInt3(VInt3.Dot(global, right) / _localScale.x, VInt3.Dot(global, up) / _localScale.y, VInt3.Dot(global, forward) / _localScale.z);
            }
        }
    }
}
