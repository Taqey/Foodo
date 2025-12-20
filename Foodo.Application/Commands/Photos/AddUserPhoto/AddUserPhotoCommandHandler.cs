using Foodo.Application.Abstraction.InfrastructureRelatedServices.Upload;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.User;
using Foodo.Application.Models.Response;
using Foodo.Domain.Entities;
using Foodo.Domain.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foodo.Application.Commands.Photos.AddUserPhoto
{
	public class AddUserPhotoCommandhandler : IRequestHandler<AddUserPhotoCommand, ApiResponse>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPhotoAccessorService _photoAccessor;
		private readonly IUserService _userService;

		public AddUserPhotoCommandhandler(IUnitOfWork unitOfWork, IPhotoAccessorService photoAccessor, IUserService userService)
		{
			_unitOfWork = unitOfWork;
			_photoAccessor = photoAccessor;
			_userService = userService;
		}
		public async Task<ApiResponse> Handle(AddUserPhotoCommand input, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.UserCustomRepository.ReadMerchants().Where(e => e.Id == input.Id).FirstOrDefaultAsync();
			var result = await _photoAccessor.AddPhoto(input.Id, input.UserType, input.file);
			if (!result.IsSuccess)
			{
				return new ApiResponse { IsSuccess = result.IsSuccess, Message = result.Message };
			}
			if (user.UserPhoto == null)
				user.UserPhoto = new LkpUserPhoto();
			user.UserPhoto.Url = result.Url;
			var UpdateResult = await _userService.UpdateAsync(user);
			if (!UpdateResult.Succeeded)
			{
				return new ApiResponse { IsSuccess = false, Message = "Adding Image to DB failed" };
			}
			return new ApiResponse { IsSuccess = true, Message = "Image Added successfully" };
		}
	}
}
