using AutoMapper;
using BusinessObject.DTOs.Request.Packages;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Packages;
using BusinessObject.Model;
using Common.Constants;
using Repository.IRepositories;

namespace Service
{
    public class PackageService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IUserPackageRepository _userPackageRepository;
        private readonly IMapper _mapper;

        public PackageService(IPackageRepository packageRepository, IMapper mapper, IUserPackageRepository userPackageRepository)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
            _userPackageRepository = userPackageRepository;
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

        public async Task<BaseResponse<PaginatedResponse<PackageQueryResponse>>> GetAllPackagesAsync(PackageQueryRequest request)
        {
            // Build filter expression
            var filter = request.BuildFilter<Package>();

            // Get total count first (for pagination info)
            var totalCount = await _packageRepository.CountAsync(filter);

            // Get filtered data with pagination
            var items = await _packageRepository.FindWithPagingAndCountAsync(
            filter,
                request.Page,
                request.GetPageSize()
            );

            var pagedItems = items.Items
                .Select(x => new PackageQueryResponse
                {
                    PackageId = x.PackageId,
                    Title = x.Title,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    IsMockTest = x.IsMockTest,
                    Price = x.Price,
                    Slot = x.Slot
                })
                .ToList();

            // Apply sorting to the paged results
            var sortedItems = request.ApplySorting(pagedItems);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

            // Map to response
            return new BaseResponse<PaginatedResponse<PackageQueryResponse>>
            {
                Data = new PaginatedResponse<PackageQueryResponse>
                {
                    Items = pagedItems,
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

        public async Task<BaseResponse<PaginatedResponse<PackageOwnedQueryResponse>>> GetOwnedPackageAsync(PackageOwnedQueryRequest request)
        {
            // Build filter expression
            var filter = request.BuildFilter<Userpackage>();

            // Get total count first (for pagination info)
            var totalCount = await _userPackageRepository.CountAsync(filter);

            // Get filtered data with pagination
            var items = await _userPackageRepository.FindWithIncludePagingAndCountAsync(
            filter,
                request.Page,
                request.GetPageSize(),
                x => x.Package
            );

            // Apply sorting to the paged results
            var sortedItems = request.ApplySorting(items.Items);

            var itemList = sortedItems
                .Select(token => new PackageOwnedQueryResponse
                {
                    PackageId = token.PackageId,
                    Title = token.Package.Title,
                    Description = token.Package.Description,
                    Price = token.Package.Price,
                    Slot = token.Package.Slot,
                    IsMockTest = token.Package.IsMockTest,
                    BuyDate = token.CreatedAt,
                    BuyPrice = token.Price,
                })
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.GetPageSize());

            // Map to response
            return new BaseResponse<PaginatedResponse<PackageOwnedQueryResponse>>
            {
                Data = new PaginatedResponse<PackageOwnedQueryResponse>
                {
                    Items = itemList,
                    TotalCount = totalCount,
                    Page = request.Page,
                    Size = request.GetPageSize(),
                    TotalPages = totalPages
                },
                Message = "Lấy toàn bộ package thành công",
                StatusCode = StatusCodeEnum.OK_200
            };
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
