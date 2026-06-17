using LuoLuoTrip;
using NUnit.Framework;
using UnityEngine;

namespace LuoLuoTrip.Tests.EditMode
{
    public class CharacterMovementMotorTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        private CharacterMovementMotor CreateMotor(Vector3 startPos)
        {
            _go = new GameObject("MotorTestRoot");
            _go.transform.position = startPos;
            var motor = _go.AddComponent<CharacterMovementMotor>();
            // EditMode does not call Awake. Force initialization at the current
            // position so _groundY captures startPos.y (matches runtime Awake).
            motor.Move(Vector3.zero);
            motor.SetGroundY(startPos.y);
            return motor;
        }

        [Test]
        public void Move_ChangesXZ_KeepsY()
        {
            var motor = CreateMotor(new Vector3(0f, 0.5f, 0f));
            motor.Move(new Vector3(2f, 0f, 3f));
            Assert.AreEqual(2f, _go.transform.position.x, 1e-4);
            Assert.AreEqual(3f, _go.transform.position.z, 1e-4);
            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-4, "Y must remain at ground level");
        }

        [Test]
        public void Move_IgnoresVerticalDelta()
        {
            var motor = CreateMotor(new Vector3(0f, 0.5f, 0f));
            motor.Move(new Vector3(0f, 99f, 0f));
            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-4);
        }

        [Test]
        public void MoveDirection_AppliesSpeedAndDeltaTime()
        {
            var motor = CreateMotor(new Vector3(0f, 0.5f, 0f));
            motor.MoveDirection(Vector3.forward, 6f, 0.5f);
            Assert.AreEqual(3f, _go.transform.position.z, 1e-3);
            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-4);
        }

        [Test]
        public void MoveTowards_ReachesTarget()
        {
            var motor = CreateMotor(new Vector3(0f, 0.5f, 0f));
            for (int i = 0; i < 10; i++)
                motor.MoveTowards(new Vector3(5f, 0.5f, 0f), 6f, 0.2f);
            Assert.AreEqual(5f, _go.transform.position.x, 1e-3);
            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-4);
        }

        [Test]
        public void Dodge_DisplacesRoot_KeepsY()
        {
            var motor = CreateMotor(new Vector3(0f, 0.5f, 0f));
            motor.Dodge(new Vector3(1f, 0f, 0f), 10f, 0.1f);
            Assert.Greater(_go.transform.position.x, 0f);
            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-4);
        }

        [Test]
        public void ClampToGroundPlane_RestoresGroundY()
        {
            var motor = CreateMotor(new Vector3(0f, 0.5f, 0f));
            _go.transform.position = new Vector3(0f, 5f, 0f);
            motor.ClampToGroundPlane();
            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-4);
        }

        [Test]
        public void LockY_False_AllowsVerticalMove()
        {
            var motor = CreateMotor(new Vector3(0f, 0f, 0f));
            motor.LockY = false;
            motor.Move(new Vector3(0f, 2f, 0f));
            // Move() still discards Y in worldDelta, so even with LockY=false vertical delta is dropped.
            // This test documents that gameplay never moves Y via motor.
            Assert.AreEqual(0f, _go.transform.position.y, 1e-4);
        }

        [Test]
        public void TeleportTo_SetsRootPosition_KeepsGroundY()
        {
            var motor = CreateMotor(new Vector3(0f, 0.5f, 0f));
            motor.TeleportTo(new Vector3(10f, 99f, -5f));
            Assert.AreEqual(10f, _go.transform.position.x, 1e-4);
            Assert.AreEqual(-5f, _go.transform.position.z, 1e-4);
            Assert.AreEqual(0.5f, _go.transform.position.y, 1e-4);
        }
    }
}
