import 'reflect-metadata';
import { container } from 'tsyringe';
import { HttpClientInstance } from '@/services/http-client.service';

// Employees
import { EmployeesRepository } from '@/features/employees/data/employees.api';
import { EmployeesService } from '@/features/employees/services/employees.service';

// Orders
import { OrdersRepository } from '@/features/orders/data/orders.api';
import { OrdersService } from '@/features/orders/services/orders.service';

// Clients
import { ClientsRepository } from '@/features/clients/data/clients.api';
import { ClientsService } from '@/features/clients/services/clients.service';

// Providers
import { ProvidersRepository } from '@/features/providers/data/providers.api';
import { ProvidersService } from '@/features/providers/services/providers.service';

// General
import { GeneralRepository } from '@/features/general/data/general.api';
import { GeneralService } from '@/features/general/services/general.service';

container.register('HttpClient', { useValue: HttpClientInstance });

// Register repositories
container.registerSingleton(EmployeesRepository);
container.registerSingleton(OrdersRepository);
container.registerSingleton(ClientsRepository);
container.registerSingleton(ProvidersRepository);
container.registerSingleton(GeneralRepository);

// Register services
container.registerSingleton(EmployeesService);
container.registerSingleton(OrdersService);
container.registerSingleton(ClientsService);
container.registerSingleton(ProvidersService);
container.registerSingleton(GeneralService);

export { container };
