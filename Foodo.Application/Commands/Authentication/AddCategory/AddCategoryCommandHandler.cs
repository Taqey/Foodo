using Foodo.Application.Abstraction.InfrastructureRelatedServices.User;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using MediatR;

namespace Foodo.Application.Commands.Authentication.AddCategory
{
	public class AddCategoryCommandHandler : IRequestHandler<AddCategoryCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserService _userService;

		public AddCategoryCommandHandler(IUnitOfWork unitOfWork, IUserService userService)
		{
			_unitOfWork = unitOfWork;
			_userService = userService;
		}
		public async Task<ApiResponse> Handle(AddCategoryCommand input, CancellationToken cancellationToken)
		{
			var user = _unitOfWork.UserCustomRepository.ReadMerchants().Where(e => e.Id == input.UserId).FirstOrDefault();
			//var user=await _userService.GetByIdAsync(input.UserId);
			foreach (var category in input.restaurantCategories)
			{
				user.TblMerchant.TblRestaurantCategories.Add(new TblRestaurantCategory
				{
					categoryid = (int)category
				});

			}
			await _userService.UpdateAsync(user);
			return ApiResponse.Success("Categories added successfully.");
		}
	}
}
