/**
 * Utility functions for Ant Design Select components
 */

/**
 * Default filter function for Select components with showSearch
 * Filters options based on their label in a case-insensitive manner
 *
 * @example
 * <Select
 *   showSearch
 *   filterOption={defaultSelectFilter}
 *   options={options}
 * />
 */
export const defaultSelectFilter = (input: string, option?: { label: string; value: string | number }) => {
  return (option?.label ?? '').toLowerCase().includes(input.toLowerCase());
};
