using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;

namespace TaskTrack.Repo.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        System.Threading.Tasks.Task<IEnumerable<T>> GetAllAsync();
        System.Threading.Tasks.Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        System.Threading.Tasks.Task<T?> GetByIdAsync(object id);
        System.Threading.Tasks.Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        System.Threading.Tasks.Task SaveChangesAsync();
    }
}
