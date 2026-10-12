using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service.Implementations
{
    public class TagService : ITagService
    {
        private readonly IGenericRepository<Tag> _tagRepo;
        private readonly TaskmanagementDbEgrzContext _context;

        public TagService(IGenericRepository<Tag> tagRepo, TaskmanagementDbEgrzContext context)
        {
            _tagRepo = tagRepo;
            _context = context;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepo.GetAllAsync();
        }

        public async Task<Tag?> GetTagByIdAsync(int id)
        {
            return await _tagRepo.GetByIdAsync(id);
        }

        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            await _tagRepo.AddAsync(tag);
            await _tagRepo.SaveChangesAsync();
            return tag;
        }

        public async Task<Tag?> UpdateTagAsync(int id, Tag updatedData)
        {
            var tag = await _tagRepo.GetByIdAsync(id);
            if (tag == null) return null;

            tag.TagName = updatedData.TagName;
            tag.Color = updatedData.Color;

            _tagRepo.Update(tag);
            await _tagRepo.SaveChangesAsync();
            return tag;
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _context.Tags.Include(t => t.Tasks).FirstOrDefaultAsync(t => t.TagId == id);
            if (tag == null) return false;

            if (tag.Tasks != null && tag.Tasks.Any())
            {
                throw new InvalidOperationException("Cannot delete tag because it is used by one or more tasks.");
            }

            _tagRepo.Delete(tag);
            await _tagRepo.SaveChangesAsync();
            return true;
        }
    }
}
