using System;
using System.Linq;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using AccountsData.Models.DataModels.Implementations.Properties;
using AccountsData.Models.DataModels.Implementations.Roles;
using AccountsData.Models.DataModels.Implementations.RoleScope;
using AccountsData.Models.DataModels.RoleProperties;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("[area]/[controller]/[action]")]
    public class dummycontroller : ControllerBase
    {
        private readonly ApplicationDbContext appContext;
        private readonly UserManager<ApplicationUser> userManager;

        public dummycontroller(ApplicationDbContext appContext, UserManager<ApplicationUser> userManager)
        {
            this.appContext = appContext;
            this.userManager = userManager;

        }
        
        [HttpGet]
        public async Task<IActionResult> OnGet()
        {
            var user3 = await userManager.FindByEmailAsync("grzesiuspam@gmail.com");
            var authorityProperty = new AuthorityProperty(1);
            var boardroleProperties = new GenericRoleProperties(0, authorityProperty);
            boardroleProperties[AuthorityProperty.Name] += new AuthorityProperty(5);
            var globalRoleProperties = new GenericRoleProperties(15, new AuthorityProperty(1000), new MayManageRolesProperty(true));
            var testBoard = appContext.Boards.Where(board => board.Name == "Nowy board").First();
            var roleProto = new BoardRole(boardroleProperties, testBoard, user3, "newrole");
            var globalRoleProto = new GlobalRole(new GenericRoleProperties(15, new AuthorityProperty(1000), new MayManageRolesProperty(true)), user3, "Testrole");
            var bannedRoleProto = new BannedRole(new GlobalScope());
            appContext.Add(roleProto);
            appContext.Add(globalRoleProto);
            appContext.Add(bannedRoleProto);
            await appContext.SaveChangesAsync();
            var role = appContext.BoardsRoles.Where(role => role.Id == roleProto.Id).First();
            var globalRole = appContext.BoardsRoles.Where(role => role.Id == globalRoleProto.Id).First();
            var bannedrole = appContext.BoardsRoles.Where(role => role.Id == bannedRoleProto.Id).First();
            user3.AddRole(role);
            user3.AddRole(globalRole);
            await appContext.SaveChangesAsync();
            Console.WriteLine(user3.MayManageRole(globalRole));
            Console.WriteLine(user3.MayManageRole(role));
            user3.AddRole(bannedrole);
            await appContext.SaveChangesAsync();
            Console.WriteLine(user3.MayManageRole(globalRole));
            Console.WriteLine(user3.MayManageRole(role));
            user3.RemoveRoles(bannedrole);
            await appContext.SaveChangesAsync();
            Console.WriteLine(user3.MayManageRole(globalRole));
            Console.WriteLine(user3.MayManageRole(role));

            globalRole.properties =
                new GenericRoleProperties(15, new AuthorityProperty(10), new MayManageRolesProperty(true));
            Console.WriteLine(user3.MayManageRole(globalRole));
            Console.WriteLine(user3.MayManageRole(role));

            return Ok("blahbal");
        }
    }
}