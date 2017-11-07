using FriendOrganizer.DataAccess;
using FriendOrganizer.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendOrganizer.UI.Data.Lookups
{
    public class LookupDataService : IFriendLookupDataService, IProgrammingLanguageLookupDataService, IMeetingLookupDataService
    {
        private Func<FriendOrganizerDbContext> _contextCreator;

        public LookupDataService(Func<FriendOrganizerDbContext> contextCreator)
        {
            _contextCreator = contextCreator;
        }

        public async Task<IEnumerable<LookupItem>> GetFriendLookupAsync()
        {
            using (var context = _contextCreator())
            {
                return await context.Friends.AsNoTracking()
                    .Select(f => 
                    new LookupItem
                    {
                        Id = f.Id,
                        DisplayMember = f.FirstName + " " + f.LastName
                    })
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<LookupItem>> GetProgrammingLanguageLookupAsync()
        {
            using (var context = _contextCreator())
            {
                return await context.ProgrammingLanguages.AsNoTracking()
                    .Select(pl =>
                    new LookupItem
                    {
                        Id = pl.Id,
                        DisplayMember = pl.Name
                    })
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<LookupItem>> GetMeetingLookupAsync()
        {
            using (var context = _contextCreator())
            {
                return await context.Meetings.AsNoTracking()
                    .Select(m =>
                    new LookupItem
                    {
                        Id = m.Id,
                        DisplayMember = m.Title
                    })
                    .ToListAsync();
            }
        }
    }
}
