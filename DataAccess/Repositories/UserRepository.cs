using DataAccess.Entities;
using DataAccess.Interfaces;

namespace DataAccess.Repositories;

public interface IUserRepository : IRepository<User, string>;
public class UserRepository(ApplicationContext context) : Repository<User, string>(context), IUserRepository;
