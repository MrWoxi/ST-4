using Stateless;

namespace BugPro
{
    public class Bug
    {
        public enum State
        {
            Open,
            Assigned,
            Deferred,
            InProgress,
            NotReproducible,
            Rejected,
            Resolved,
            Reopened,
            Closed
        }

        public enum Trigger
        {
            Assign,
            Defer,
            Resume,
            StartFix,
            Reject,
            CannotReproduce,
            Confirm,
            Deny,
            Resolve,
            Verify,
            Reopen,
            ReturnToTriage
        }

        private readonly StateMachine<State, Trigger> _machine;

        public Bug(State initial = State.Open)
        {
            _machine = new StateMachine<State, Trigger>(initial);

            _machine.Configure(State.Open)
                .Permit(Trigger.Assign, State.Assigned);

            _machine.Configure(State.Assigned)
                .Permit(Trigger.StartFix,         State.InProgress)
                .Permit(Trigger.Defer,            State.Deferred)
                .Permit(Trigger.Reject,           State.Rejected)
                .Permit(Trigger.CannotReproduce,  State.NotReproducible);

            _machine.Configure(State.Deferred)
                .Permit(Trigger.Resume, State.Assigned);

            _machine.Configure(State.NotReproducible)
                .Permit(Trigger.Confirm, State.Closed)
                .Permit(Trigger.Deny,    State.Assigned);

            _machine.Configure(State.Rejected)
                .Permit(Trigger.Reopen, State.Assigned)
                .Permit(Trigger.Verify, State.Closed);

            _machine.Configure(State.InProgress)
                .Permit(Trigger.Resolve, State.Resolved);

            _machine.Configure(State.Resolved)
                .Permit(Trigger.Verify, State.Closed)
                .Permit(Trigger.Reopen, State.Reopened);

            _machine.Configure(State.Reopened)
                .Permit(Trigger.ReturnToTriage, State.Assigned);

            _machine.Configure(State.Closed)
                .Permit(Trigger.Reopen, State.Reopened);
        }

        public State CurrentState => _machine.State;

        public bool CanFire(Trigger trigger) => _machine.CanFire(trigger);

        public void Fire(Trigger trigger) => _machine.Fire(trigger);

        public void Assign()           => Fire(Trigger.Assign);
        public void Defer()            => Fire(Trigger.Defer);
        public void Resume()           => Fire(Trigger.Resume);
        public void StartFix()         => Fire(Trigger.StartFix);
        public void Reject()           => Fire(Trigger.Reject);
        public void CannotReproduce()  => Fire(Trigger.CannotReproduce);
        public void Confirm()          => Fire(Trigger.Confirm);
        public void Deny()             => Fire(Trigger.Deny);
        public void Resolve()          => Fire(Trigger.Resolve);
        public void Verify()           => Fire(Trigger.Verify);
        public void Reopen()           => Fire(Trigger.Reopen);
        public void ReturnToTriage()   => Fire(Trigger.ReturnToTriage);
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var bug = new Bug();
            Console.WriteLine($"Создан баг. Состояние: {bug.CurrentState}");
            bug.Assign();
            Console.WriteLine($"Назначен: {bug.CurrentState}");
            bug.StartFix();
            Console.WriteLine($"В работе: {bug.CurrentState}");
            bug.Resolve();
            Console.WriteLine($"Исправлен: {bug.CurrentState}");
            bug.Verify();
            Console.WriteLine($"Закрыт: {bug.CurrentState}");
            Console.WriteLine($"Можно переоткрыть? {bug.CanFire(Bug.Trigger.Reopen)}");
        }
    }
}
