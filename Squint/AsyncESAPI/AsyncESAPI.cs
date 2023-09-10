using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.ComponentModel;
using VMS.TPS.Common.Model.API;
using EApp = VMS.TPS.Common.Model.API.Application;
using System.Collections.Concurrent;


namespace Squint
{
    public class AsyncESAPI : IDisposable
    {
        class AppTaskScheduler : TaskScheduler, IDisposable
        {
            private BlockingCollection<Task> m_tasks;
            private Thread m_thread;

            public AppTaskScheduler()
            {
                m_tasks = new BlockingCollection<Task>();
                m_thread = new Thread(() =>
                {
                    foreach (var task in m_tasks.GetConsumingEnumerable())
                        TryExecuteTask(task);
                })
                {
                    IsBackground = true
                };
                m_thread.SetApartmentState(ApartmentState.STA);
                m_thread.Start();
            }

            public void Dispose()
            {
                if (m_tasks != null)
                {
                    m_tasks.CompleteAdding();
                    m_thread.Join();
                    m_tasks.Dispose();
                    m_thread = null;
                    m_tasks = null;
                }
            }

            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return m_tasks.ToArray();
            }

            protected override void QueueTask(Task task)
            {
                m_tasks.Add(task);
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                return Thread.CurrentThread.GetApartmentState() == ApartmentState.STA && TryExecuteTask(task);
            }

            public override int MaximumConcurrencyLevel => 1;
        }
        private readonly AppTaskScheduler m_taskScheduler = new AppTaskScheduler();
        private EApp m_application;
        private Patient _Patient = null;
        public AsyncPatient Patient = null;
        public bool isInit { get; private set; } = false;

        public AsyncESAPI(string username = null, string password = null)
        {
            m_application = Execute(new Func<EApp>(() =>
            {
                try
                {
                    var app = EApp.CreateApplication();
                    return app;
                }
                catch (Exception ex)
                {
                    throw new Exception();
                }
            }));
            isInit = true;
        }

        public bool OpenPatient(string PID)
        {
            if (!isInit)
                return false;
            if (_Patient != null)
            {
                Execute(new Action<EApp>((application) =>
                {
                    application.ClosePatient();
                    _Patient = null;
                    Patient = null;
                }));
            }
            Execute(new Action<EApp>((application) =>
            {
                if (_Patient == null)
                    _Patient = application.OpenPatientById(PID);
                if (_Patient != null)
                    Patient = new AsyncPatient(this, _Patient);
                else
                    Patient = null;
                return;
            }
            ));
            if (Patient == null)
                return false;
            else return true;
        }

        public void ClosePatient()
        {
            if (!isInit)
                return;
            if (_Patient != null)
            {
                Execute(new Action<EApp>((application) =>
                {
                    application.ClosePatient();
                    _Patient = null;
                    Patient = null;
                }));
            }
        }
        public string CurrentUserId()
        {
            if (!isInit)
                return "nchng"; // return me by default
            return Execute(new Func<EApp, string>((application) =>
            {
                return application.CurrentUser.Id;
            }));
        }
        public string CurrentUserName()
        {
            if (!isInit)
                return "nchng"; // return me by default
            return Execute(new Func<EApp, string>((application) =>
            {
                return application.CurrentUser.Name;
            }));
        }
        public async Task<T> ExecuteAsync<T>(Func<EApp, T> func)
        {
            return await Task.Factory.StartNew(() => func.Invoke(m_application), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<Structure, T> func, Structure s)
        {
            return await Task.Factory.StartNew(() => func.Invoke(s), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<PlanSetup, T> func, PlanSetup p)
        {
            return await Task.Factory.StartNew(() => func.Invoke(p), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<Structure, PlanSetup, T> func, Structure s, PlanSetup p)
        {
            return await Task.Factory.StartNew(() => func.Invoke(s, p), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<PlanSetup, Patient, T> func, PlanSetup p, Patient pt)
        {
            return await Task.Factory.StartNew(() => func.Invoke(p, pt), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<PlanSum, T> func, PlanSum ps)
        {
            return await Task.Factory.StartNew(() => func.Invoke(ps), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<Patient, T> func)
        {
            return await Task.Factory.StartNew(() => func.Invoke(_Patient), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }

        public T Execute<T>(Func<EApp, T> func)
        {
            return Task.Run(async () => await ExecuteAsync(func)).Result;
        }
        public T Execute<T>(Func<Patient, T> func)
        {
            return Task.Run(async () => await ExecuteAsync(func)).Result;
        }
        protected T Execute<T>(Func<T> func)
        {
            return Task.Run(async () => await Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, m_taskScheduler)).Result;
        }

        public async Task ExecuteAsync(Action<EApp> func)
        {
            await Task.Factory.StartNew(() => func.Invoke(m_application), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }

        public void Execute(Action<EApp> func)
        {
            Task.Run(async () => await ExecuteAsync(func)).Wait();
        }

        public void Dispose()
        {
            if (m_application != null)
            {
                Task.Factory.StartNew(() => m_application.Dispose(), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
                m_taskScheduler.Dispose();
            }
        }
    }
}
