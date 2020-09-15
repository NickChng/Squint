using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using VMS.TPS.Common.Model.API;
using EApp = VMS.TPS.Common.Model.API.Application;


namespace SquintScript
{
    public class AsyncPatient
    {
        private AsyncESAPI A;
        //public event EventHandler AsyncPatientClosing;
        public string Id { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public List<string> CourseIds { get; private set; } = new List<string>();
        public List<string> StructureSetUIDs { get; private set; } = new List<string>();

        private Dictionary<string, AsyncCourse> _Courses = new Dictionary<string, AsyncCourse>();

        private List<AsyncStructureSet> cachedASS = new List<AsyncStructureSet>();
        public AsyncPatient(AsyncESAPI ACurrent, Patient p)
        {
            A = ACurrent;
            Id = p.Id;
            FirstName = p.FirstName;
            LastName = p.LastName;
            foreach (Course c in p.Courses)
            {
                CourseIds.Add(c.Id);
            }
            foreach (StructureSet SS in p.StructureSets)
            {
                StructureSetUIDs.Add(SS.UID);
            }
        }
        public AsyncStructureSet GetStructureSet(string ssuid)
        {
            var ASS = cachedASS.FirstOrDefault(x => x.UID == ssuid);
            if (ASS != null)
                return ASS;
            else
            {
                if (StructureSetUIDs.Contains(ssuid))
                {
                    ASS = A.Execute(new Func<Patient, AsyncStructureSet>((p) =>
                    {
                        return new AsyncStructureSet(A, p.StructureSets.FirstOrDefault(x => x.UID == ssuid));
                    }));
                    if (ASS != null)
                        cachedASS.Add(ASS);
                    return ASS;
                }
                else return null;
            }
        }
        public async Task<AsyncCourse> GetCourse(string CourseId)
        {
            if (!CourseIds.Contains(CourseId))
                return null;
            if (_Courses.ContainsKey(CourseId))
                return _Courses[CourseId];
            else
            {
                AsyncCourse c = await A.ExecuteAsync(new Func<Patient, AsyncCourse>((p) =>
                {
                    if (p.Courses.Select(x => x.Id).Contains(CourseId))
                    {
                        Course C = p.Courses.Where(x => x.Id == CourseId).Single();
                        return new AsyncCourse(A, C, p);
                    }
                    else
                        return null;
                }));
                _Courses.Add(CourseId, c);
                if (c != null)
                    return c;
                else
                    return null;
            }
        }
    }
}
