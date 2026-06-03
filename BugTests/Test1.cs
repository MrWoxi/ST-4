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
        public void Open_Assign_GoesToAssigned()
        {
            var bug = new Bug();
            bug.Assign();
            Assert.AreEqual(Bug.State.Assigned, bug.CurrentState);
        }

        [TestMethod]
        public void Assigned_StartFix_GoesToInProgress()
        {
            var bug = new Bug(Bug.State.Assigned);
            bug.StartFix();
            Assert.AreEqual(Bug.State.InProgress, bug.CurrentState);
        }

        [TestMethod]
        public void InProgress_Resolve_GoesToResolved()
        {
            var bug = new Bug(Bug.State.InProgress);
            bug.Resolve();
            Assert.AreEqual(Bug.State.Resolved, bug.CurrentState);
        }

        [TestMethod]
        public void Resolved_Verify_GoesToClosed()
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
        public void Assigned_Defer_Resume()
        {
            var bug = new Bug(Bug.State.Assigned);
            bug.Defer();
            Assert.AreEqual(Bug.State.Deferred, bug.CurrentState);
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
        public void Assigned_CannotReproduce()
        {
            var bug = new Bug(Bug.State.Assigned);
            bug.CannotReproduce();
            Assert.AreEqual(Bug.State.NotReproducible, bug.CurrentState);
        }

        [TestMethod]
        public void NotReproducible_Confirm_Deny()
        {
            var bug1 = new Bug(Bug.State.NotReproducible);
            bug1.Confirm();
            Assert.AreEqual(Bug.State.Closed, bug1.CurrentState);

            var bug2 = new Bug(Bug.State.NotReproducible);
            bug2.Deny();
            Assert.AreEqual(Bug.State.Assigned, bug2.CurrentState);
        }

        [TestMethod]
        public void Closed_Reopen_ReturnToTriage()
        {
            var bug = new Bug(Bug.State.Closed);
            bug.Reopen();
            Assert.AreEqual(Bug.State.Reopened, bug.CurrentState);
            bug.ReturnToTriage();
            Assert.AreEqual(Bug.State.Assigned, bug.CurrentState);
        }

        [TestMethod]
        public void ReopenedCanBeClosedAgain()
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
        public void CanFire_OpenAllowsAssign()
        {
            var bug = new Bug();
            Assert.IsTrue(bug.CanFire(Bug.Trigger.Assign));
            Assert.IsFalse(bug.CanFire(Bug.Trigger.Resolve));
        }

        [TestMethod]
        public void CanFire_ClosedAllowsReopen()
        {
            var bug = new Bug(Bug.State.Closed);
            Assert.IsTrue(bug.CanFire(Bug.Trigger.Reopen));
            Assert.IsFalse(bug.CanFire(Bug.Trigger.Verify));
        }

        [TestMethod]
        public void InvalidTransitions_Throw()
        {
            var bug = new Bug();
            Assert.ThrowsExactly<InvalidOperationException>(() => bug.Resolve());
            Assert.ThrowsExactly<InvalidOperationException>(() => bug.StartFix());
        }

        [TestMethod]
        public void InvalidTransition_MessageMentionsState()
        {
            var bug = new Bug();
            var ex = Assert.ThrowsExactly<InvalidOperationException>(() => bug.Resolve());
            StringAssert.Contains(ex.Message, "Open");
        }
    }
}
