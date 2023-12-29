using DataAccess.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DataAccess.Entities;

public class User : IdentityUser, IEntity<string>
{
    
}
