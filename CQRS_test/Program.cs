using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS_test
{
    public class Person
    {
        private int age = 20;
        private EventBroker eb;

        public Person(EventBroker eventBroker)
        {
            eb = eventBroker;
            eb.Commands += Eb_Commands;
            eb.Queries += Eb_Queries;
        }

        private void Eb_Queries(object sender, Query e)
        {
            var aq = e as AgeQuery;
            if (aq != null && aq.Target == this)
            {
                e.Result = age;
            }
        }

        private void Eb_Commands(object sender, Command e)
        {
            var cac = e as ChangeAgeCommand;
            if (cac != null && cac.Target == this)
            {
                eb.AllEvents.Add(new AgeChangedEvent(this, age, cac.Age));
                age = cac.Age;
            }
        }
    }

    public class EventBroker
    {
        public List<Event> AllEvents = new List<Event>();

        public event EventHandler<Command> Commands;

        public event EventHandler<Query> Queries;

        public void Command(Command c)
        {
            Commands?.Invoke(this, c);
        }

        public T Query<T>(Query q)
        {
            Queries?.Invoke(this, q);
            return (T)q.Result;
        }
    }

    public class Command : EventArgs
    {
    }

    class ChangeAgeCommand : Command
    {
        public Person Target;
        public int Age;
        public ChangeAgeCommand(Person target, int age)
        {
            Target = target;
            this.Age = age;
        }

        
    }

    public class Query
    {
        public object Result;
    }

    class AgeQuery : Query
    {
        public Person Target;

        public AgeQuery(Person target)
        {
            Target = target;
        }
    }

    public class Event
    {
    }

    class AgeChangedEvent : Event
    {
        public Person Target;
        public int NewValue, OldValue;

        public AgeChangedEvent(Person target, int oldValue, int newValue)
        {
            Target = target;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public override string ToString()
        {
            return String.Format("Age was {0} and now is {1}", OldValue, NewValue);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            EventBroker eb = new EventBroker();
            Person p = new Person(eb);

            Console.WriteLine(eb.Query<int>(new AgeQuery(p)));
            eb.Command(new ChangeAgeCommand(p, 30));
            Console.WriteLine(eb.Query<int>(new AgeQuery(p)));

            eb.AllEvents.ForEach(x => Console.WriteLine(x.ToString()));

            Console.ReadLine();
        }
    }
}
