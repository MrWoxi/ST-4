using BugPro;

namespace BugTests
{
    [TestClass]
    public sealed class BugStateTests
    {
        [TestMethod]
        public void NewBug_IsOpen()
        {
            var bug = new Bug();
            Assert.AreEqual(Bug.State.Open, bug.CurrentState);
        }

        [TestMethod]
        public void NewBug_ExplicitState()
        {
            var bug = new Bug(Bug.State.Assigned);
            Assert.AreEqual(Bug.State.Assigned, bug.CurrentState);
        }

        [TestMethod]
        public void Open_Assign()
        {
            var bug = new Bug();
            bug.Assign();
            Assert.AreEqual(Bug.State.Assigned, bug.CurrentState);
        }

        [TestMethod]
        public void Assigned_StartFix()
        {
            var bug = new Bug(Bug.State.Assigned);
            bug.StartFix();
            Assert.AreEqual(Bug.State.InProgress, bug.CurrentState);
        }

        [TestMethod]
        public void InProgress_Resolve()
        {
            var bug = new Bug(Bug.State.InProgress);
            bug.Resolve();
            Assert.AreEqual(Bug.State.Resolved, bug.CurrentState);
        }

        [TestMethod]
        public void Resolved_Verify()
        {
            var bug = new Bug(Bug.State.Resolved);
            bug.Verify();
            Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
        }

        [TestMethod]
        public void FullHappyPath()
        {
            var bug = new Bug();
            bug.Assign();
            bug.StartFix();
            bug.Resolve();
            bug.Verify();
            Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
        }

        [TestMethod]
        public void Assigned_Defer()
        {
            var bug = new Bug(Bug.State.Assigned);
            bug.Defer();
            Assert.AreEqual(Bug.State.Deferred, bug.CurrentState);
        }

        [TestMethod]
        public void Deferred_Resume()
        {
            var bug = new Bug(Bug.State.Deferred);
            bug.Resume();
            Assert.AreEqual(Bug.State.Assigned, bug.CurrentState);
        }

        [TestMethod]
        public void Assigned_Reject()
        {
            var bug = new Bug(Bug.State.Assigned);
            bug.Reject();
            Assert.AreEqual(Bug.State.Rejected, bug.CurrentState);
        }

        [TestMethod]
        public void Rejected_Reopen()
        {
            var bug = new Bug(Bug.State.Rejected);
            bug.Reopen();
            Assert.AreEqual(Bug.State.Assigned, bug.CurrentState);
        }

        [TestMethod]
        public void Rejected_Verify()
        {
            var bug = new Bug(Bug.State.Rejected);
            bug.Verify();
            Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
        }

        [TestMethod]
        public void Assigned_CannotReproduce()
        {
            var bug = new Bug(Bug.State.Assigned);
            bug.CannotReproduce();
            Assert.AreEqual(Bug.State.NotReproducible, bug.CurrentState);
        }

        [TestMethod]
        public void NotReproducible_Confirm()
        {
            var bug = new Bug(Bug.State.NotReproducible);
            bug.Confirm();
            Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
        }

        [TestMethod]
        public void NotReproducible_Deny()
        {
            var bug = new Bug(Bug.State.NotReproducible);
            bug.Deny();
            Assert.AreEqual(Bug.State.Assigned, bug.CurrentState);
        }

        [TestMethod]
        public void Resolved_Reopen()
        {
            var bug = new Bug(Bug.State.Resolved);
            bug.Reopen();
            Assert.AreEqual(Bug.State.Reopened, bug.CurrentState);
        }

        [TestMethod]
        public void Closed_Reopen()
        {
            var bug = new Bug(Bug.State.Closed);
            bug.Reopen();
            Assert.AreEqual(Bug.State.Reopened, bug.CurrentState);
        }

        [TestMethod]
        public void Reopened_ReturnToTriage()
        {
            var bug = new Bug(Bug.State.Reopened);
            bug.ReturnToTriage();
            Assert.AreEqual(Bug.State.Assigned, bug.CurrentState);
        }

        [TestMethod]
        public void ReopenedBugCanClose()
        {
            var bug = new Bug(Bug.State.Closed);
            bug.Reopen();
            bug.ReturnToTriage();
            bug.StartFix();
            bug.Resolve();
            bug.Verify();
            Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
        }

        [TestMethod]
        public void CanFire_OpenOnlyAssign()
        {
            var bug = new Bug();
            Assert.IsTrue(bug.CanFire(Bug.Trigger.Assign));
            Assert.IsFalse(bug.CanFire(Bug.Trigger.Resolve));
            Assert.IsFalse(bug.CanFire(Bug.Trigger.StartFix));
        }

        [TestMethod]
        public void CanFire_ClosedOnlyReopen()
        {
            var bug = new Bug(Bug.State.Closed);
            Assert.IsTrue(bug.CanFire(Bug.Trigger.Reopen));
            Assert.IsFalse(bug.CanFire(Bug.Trigger.Verify));
        }

        [TestMethod]
        public void CanFire_AssignedFourOptions()
        {
            var bug = new Bug(Bug.State.Assigned);
            Assert.IsTrue(bug.CanFire(Bug.Trigger.StartFix));
            Assert.IsTrue(bug.CanFire(Bug.Trigger.Defer));
            Assert.IsTrue(bug.CanFire(Bug.Trigger.Reject));
            Assert.IsTrue(bug.CanFire(Bug.Trigger.CannotReproduce));
        }

        [TestMethod]
        public void Open_Resolve_Throws()
        {
            var bug = new Bug();
            Assert.ThrowsExactly<InvalidOperationException>(() => bug.Resolve());
        }

        [TestMethod]
        public void Open_StartFix_Throws()
        {
            var bug = new Bug();
            Assert.ThrowsExactly<InvalidOperationException>(() => bug.StartFix());
        }

        [TestMethod]
        public void Closed_Verify_Throws()
        {
            var bug = new Bug(Bug.State.Closed);
            Assert.ThrowsExactly<InvalidOperationException>(() => bug.Verify());
        }

        [TestMethod]
        public void InProgress_Verify_Throws()
        {
            var bug = new Bug(Bug.State.InProgress);
            Assert.ThrowsExactly<InvalidOperationException>(() => bug.Verify());
        }

        [TestMethod]
        public void Deferred_StartFix_Throws()
        {
            var bug = new Bug(Bug.State.Deferred);
            Assert.ThrowsExactly<InvalidOperationException>(() => bug.StartFix());
        }

        [TestMethod]
        public void ErrorMessageContainsState()
        {
            var bug = new Bug();
            var ex = Assert.ThrowsExactly<InvalidOperationException>(() => bug.Resolve());
            StringAssert.Contains(ex.Message, "Open");
        }

        [TestMethod]
        public void Fire_ValidTrigger()
        {
            var bug = new Bug();
            bug.Fire(Bug.Trigger.Assign);
            Assert.AreEqual(Bug.State.Assigned, bug.CurrentState);
        }

        [TestMethod]
        public void Fire_InvalidTrigger_Throws()
        {
            var bug = new Bug();
            Assert.ThrowsExactly<InvalidOperationException>(
                () => bug.Fire(Bug.Trigger.Resolve));
        }
    }
}
