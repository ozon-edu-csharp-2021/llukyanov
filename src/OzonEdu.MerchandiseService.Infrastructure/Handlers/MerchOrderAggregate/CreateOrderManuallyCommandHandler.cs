﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OzonEdu.MerchandiseService.Domain.AggregationModels.EmployeeAggregate;
using OzonEdu.MerchandiseService.Domain.AggregationModels.OrderAggregate;
using OzonEdu.MerchandiseService.Infrastructure.Commands.AskMerch;
using OzonEdu.MerchandiseService.Infrastructure.Commands.ReserveMerch;

namespace OzonEdu.MerchandiseService.Infrastructure.Handlers
{
    public class
        CreateOrderManuallyCommandHandler : IRequestHandler<CreateOrderManuallyRequest, CreateOrderManuallyResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IOrderRepository _OrderRepository;

        public CreateOrderManuallyCommandHandler(IOrderRepository OrderRepository,
            IEmployeeRepository employeeRepository)
        {
            _OrderRepository = OrderRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<CreateOrderManuallyResponse> Handle(CreateOrderManuallyRequest request,
            CancellationToken cancellationToken)
        {
            var orderDetails = new Order(request.EmployeeEmail, request.Skus, null, OrderPriority.Medium);
            var newOrder = await _OrderRepository.CreateAsync(orderDetails, cancellationToken);
            return new CreateOrderManuallyResponse
            {
                OrderId = newOrder.Id
            };
        }
    }
}