using GameManager.DB.Enums;
using GameManager.DB.Models;
using GameManager.DTOs;
using System.Linq.Expressions;

namespace GameManager.Services
{
    /// <summary>
    /// this class is usage for game info provider to access database
    /// </summary>
    /// <param name="configService"></param>
    public class StaffService(IConfigService configService) : IStaffService
    {
        private const string STAFF = "staff";
        private const string SCENARIO = "scenario";
        private const string DIRECTOR = "director";
        private const string CHARACTER_DESIGN = "character design";
        private const string ARTIST = "artist";
        private const string MUSIC = "music";
        private const string SONG = "song";
        private readonly Dictionary<StaffRoleEnum, StaffRoleDTO> _cacheRole = new();
        private IEnumerable<StaffRoleDTO>? _cache;

        public async Task<IEnumerable<StaffRoleDTO>> GetStaffRolesAsync()
        {
            if (_cache != null)
                return _cache;
            _cache = await configService.GetStaffRolesAsync();
            return _cache;
        }

        public async Task<StaffDTO?> GetStaffAsync(Expression<Func<Staff, bool>> expression)
        {
            return await configService.GetStaffAsync(expression);
        }

        public async Task<StaffRoleDTO> GetStaffRoleAsync(StaffRoleEnum role)
        {
            if (_cacheRole.TryGetValue(role, out StaffRoleDTO? roleCache))
                return roleCache;
            _cache ??= await configService.GetStaffRolesAsync();
            string roleString = role switch
            {
                StaffRoleEnum.SCENARIO => SCENARIO,
                StaffRoleEnum.SONG => SONG,
                StaffRoleEnum.MUSIC => MUSIC,
                StaffRoleEnum.ARTIST => ARTIST,
                StaffRoleEnum.CHARACTER_DESIGN => CHARACTER_DESIGN,
                StaffRoleEnum.DIRECTOR => DIRECTOR,
                StaffRoleEnum.STAFF => STAFF,
                _ => STAFF
            };

            _cacheRole[role] = _cache.FirstOrDefault(x => x.RoleName == roleString) ??
                               throw new ArgumentException("Staff Role not found");
            return _cacheRole[role];
        }

        public Task<StaffRoleEnum> GetStaffRoleEnumByName(string roleName)
        {
            StaffRoleEnum result = roleName switch
            {
                SCENARIO => StaffRoleEnum.SCENARIO,
                SONG => StaffRoleEnum.SONG,
                MUSIC => StaffRoleEnum.MUSIC,
                ARTIST => StaffRoleEnum.ARTIST,
                CHARACTER_DESIGN => StaffRoleEnum.CHARACTER_DESIGN,
                DIRECTOR => StaffRoleEnum.DIRECTOR,
                STAFF => StaffRoleEnum.STAFF,
                _ => StaffRoleEnum.STAFF
            };
            return Task.FromResult(result);
        }
    }
}