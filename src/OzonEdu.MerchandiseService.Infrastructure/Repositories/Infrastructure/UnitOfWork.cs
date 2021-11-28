using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Npgsql;
using OzonEdu.MerchandiseService.Domain.Contracts;
using OzonEdu.MerchandiseService.Infrastructure.Repositories.Infrastructure.Exceptions;
using OzonEdu.MerchandiseService.Infrastructure.Repositories.Infrastructure.Interfaces;

namespace OzonEdu.MerchandiseService.Infrastructure.Repositories.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IChangeTracker _changeTracker;

        private readonly IDbConnectionFactory<NpgsqlConnection> _dbConnectionFactory;
        private readonly IPublisher _publisher;
        private NpgsqlTransaction _npgsqlTransaction;

        public UnitOfWork(
            IDbConnectionFactory<NpgsqlConnection> dbConnectionFactory,
            IPublisher publisher,
            IChangeTracker changeTracker)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _publisher = publisher;
            _changeTracker = changeTracker;
        }

        void IDisposable.Dispose()
        {
            _npgsqlTransaction?.Dispose();
            _dbConnectionFactory?.Dispose();
        }

        public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async ValueTask StartTransaction(CancellationToken token)
        {
            if (_npgsqlTransaction is not null) return;
            var connection = await _dbConnectionFactory.CreateConnection(token);
            _npgsqlTransaction = await connection.BeginTransactionAsync(token);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (_npgsqlTransaction is null) throw new NoActiveTransactionStartedException();

            var domainEvents = new Queue<INotification>(
                _changeTracker.TrackedEntities
                    .SelectMany(x =>
                    {
                        var events = x.DomainEvents.ToList();
                        x.ClearDomainEvents();
                        return events;
                    }));
            // Можно отправлять все и сразу через Task.WhenAll.
            while (domainEvents.TryDequeue(out var notification))
                await _publisher.Publish(notification, cancellationToken);

            await _npgsqlTransaction.CommitAsync(cancellationToken);
        }
    }
}