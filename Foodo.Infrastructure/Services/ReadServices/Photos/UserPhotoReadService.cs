using Dapper;
using Foodo.Application.Abstraction.InfrastructureRelatedServices.ReadServices.Photos;
using Foodo.Application.Models.Dto.Photo;
using System.Data;

namespace Foodo.Infrastructure.Services.ReadServices.Photos
{
	public class UserPhotoReadService : IUserPhotoReadService
	{
		private readonly IDbConnection _connection;

		public UserPhotoReadService(IDbConnection connection)
		{
			_connection = connection;
		}
		public async Task<GetPhotoDto> ReadUserPhoto(string Id)
		{
			var sql = @"SELECT LkpUserPhotos.Url
FROM     AspNetUsers INNER JOIN
                  LkpUserPhotos ON AspNetUsers.Id = LkpUserPhotos.UserId 
                  where AspNetUsers.Id=@Id";
			var result = (await _connection.QueryAsync<GetPhotoDto>(sql, new { Id = Id })).FirstOrDefault();
			return result;
		}
	}
}
