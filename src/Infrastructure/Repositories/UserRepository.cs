using Geneirodan.Generics.Repository;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Infrastructure.Repositories;

// ReSharper disable once UnusedType.Global
public class UserRepository(ApplicationContext context) : Repository<User, string, ApplicationContext>(context), IUserRepository;
