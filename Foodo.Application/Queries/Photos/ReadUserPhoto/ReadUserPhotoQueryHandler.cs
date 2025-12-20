using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Photos;
using Foodo.Application.Models.Dto.Photo;
using Foodo.Application.Models.Response;
using Foodo.Domain.Repository;
using MediatR;

namespace Foodo.Application.Queries.Photos.ReadUserPhoto
{
	public class ReadUserPhotoQueryHandler : IRequestHandler<ReadUserPhotoQuery, ApiResponse<GetPhotoDto>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IUserPhotoReadService _service;

		public ReadUserPhotoQueryHandler(IUnitOfWork unitOfWork, IUserPhotoReadService service)
		{
			_unitOfWork = unitOfWork;
			_service = service;
		}
		public async Task<ApiResponse<GetPhotoDto>> Handle(ReadUserPhotoQuery input, CancellationToken cancellationToken)
		{
			var result = await _service.ReadUserPhoto(input.Id);
			if (result == null)
			{
				return ApiResponse<GetPhotoDto>.Failure("Image not found");
			}
			return ApiResponse<GetPhotoDto>.Success(result, "Image retrieved");

		}
	}
}
