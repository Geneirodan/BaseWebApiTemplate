using Geneirodan.Generics.Repository.Interfaces;
using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IUserRepository : IRepository<User, string>;
