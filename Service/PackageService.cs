using AutoMapper;
using BusinessObject.DTOs.Response;
using BusinessObject.Model;
using BusinessObject.Payload.Request;
using Common.Constants;
using Repository.IRepositories;

namespace Service
{
    public class PackageService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;

        public PackageService(IPackageRepository packageRepository, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<Package>> CreatePackageAsync(PackageCreateRequest request)
        {
            var package = _mapper.Map<Package>(request);
            package.CreatedAt = DateTime.UtcNow;
            package.UpdatedAt = DateTime.UtcNow;

            if(package.IsMockTest == true)
            {
                package.Slot = 0;
            }

            var result = await _packageRepository.AddAsync(package);

            return new BaseResponse<Package>(
                "Tạo package thành công",
                StatusCodeEnum.Created_201,
                _mapper.Map<Package>(result));
        }

        public async Task<BaseResponse<PaginatedResponse<Package>>> GetAllPackagesAsync(PackageQueryRequest request)
        {
            // Build filter expression
            var filter = request.BuildFilter<Package>();

            // Get total count first (for pagination info)
            var totalCount = await _packageRepository.CountAsync(filter);

            // Get filtered data with pagination
            var items = await _packageRepository.FindWithPagingAsync(
            filter,
                request.Page,
                request.GetPageSize()
            );

            // Apply sorting to the paged results
            var sortedItems = request.ApplySorting(items);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

            // Map to response
            return new BaseResponse<PaginatedResponse<Package>>
            {
                Data = new PaginatedResponse<Package>
                {
                    Items = sortedItems.Select(token => _mapper.Map<Package>(token)).ToList(),
                    TotalCount = totalCount,
                    Page = request.Page,
                    Size = request.GetPageSize(),
                    TotalPages = totalPages
                },
                Message = "Lấy toàn bộ package thành công",
                StatusCode = StatusCodeEnum.OK_200
            };
        }

        public async Task<BaseResponse<Package>> GetPackageByIdAsync(int id)
        {
            var result = await _packageRepository.GetByIdAsync(id);

            if (result == null)
            {
                throw new Exception("Không tìm thấy package");
            }

            return new BaseResponse<Package>("Lấy package thành công", StatusCodeEnum.OK_200, _mapper.Map<Package>(result));
        }

        public async Task<BaseResponse<Package>> UpdatePackageAsync(PackageUpdateRequest request)
        {
            var package = await _packageRepository.GetByIdAsync(request.PackageId);

            if (package == null)
            {
                throw new Exception("Không tìm thấy package");
            }

            if (request.IsMockTest == true)
            {
                request.Slot = 0;
            }

            package.Title = request.Title;
            package.Description = request.Description;
            package.Price = (decimal)request.Price;
            package.IsActive = request.IsActive;
            package.IsMockTest = request.IsMockTest;
            package.Slot = (int)request.Slot;
            package.UpdatedAt = DateTime.UtcNow;

            var result = await _packageRepository.UpdateAsync(package);

            return new BaseResponse<Package>("Cập nhật package thành công", StatusCodeEnum.OK_200, result);
        }
    }
}
