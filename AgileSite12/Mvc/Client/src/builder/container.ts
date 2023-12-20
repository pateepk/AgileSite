import { Container, interfaces } from "inversify";

let container: Container = null;

/**
 * Returns container with services that can be differently implemented by different types of builders.
 */
export const initContainer = (newContainer: Container): void => {
  container = newContainer;
};

/**
 * Returns service by given identifier.
 */
export const getService = <T>(serviceIdentifier: interfaces.ServiceIdentifier<T>): T => {
  return container.get<T>(serviceIdentifier);
};

/**
 * Resolves a service even if no bindings have been declared.
 */
export const resolveService = <T>(serviceConstructor: interfaces.Newable<T>) => {
  return container.resolve<T>(serviceConstructor);
};
