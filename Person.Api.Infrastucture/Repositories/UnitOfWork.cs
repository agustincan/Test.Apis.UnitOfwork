using System.Collections;
using Person.Api.Infrastucture.DataBase;
using Repository.Core;

namespace Person.Api.Infrastucture.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        //private readonly ICurrentUserService _currentUserService;
        private readonly AppDbContext _dbContext;
        private bool disposed;
        private Hashtable _repositories;

        public UnitOfWork(AppDbContext dbContext) //, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            //_currentUserService = currentUserService;
        }

        public IRepositoryAsync<TEntity> Repository<TEntity>() where TEntity : class //AuditableEntity
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(TEntity).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(RepositoryAsync<>);

                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _dbContext);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepositoryAsync<TEntity>)_repositories[type]!;
        }

        public async Task<int> Commit(CancellationToken cancellationToken)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task Rollback()
        {
            _dbContext.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //dispose managed resources
                    _dbContext.Dispose();
                }
            }
            //dispose unmanaged resources
            disposed = true;
        }
    }   
}