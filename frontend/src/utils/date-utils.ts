import dayjs, { Dayjs } from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';
import { ENV } from '@/env';

// Extend dayjs with timezone support
dayjs.extend(utc);
dayjs.extend(timezone);

/**
 * Formats a date for API submission without timezone conversion
 * This ensures the date sent to the API is exactly what the user selected
 *
 * @param date - The dayjs date object from DatePicker or a date string (YYYY-MM-DD)
 * @returns Formatted date string in YYYY-MM-DDTHH:mm:ss format
 *
 * @example
 * User selects: December 2, 2025
 * Input: "2025-12-02" or dayjs object
 * Returns: "2025-12-02T00:00:00"
 */
export function formatDateForAPI(date: Dayjs | string | null | undefined): string {
  if (!date) {
    return dayjs().format('YYYY-MM-DDTHH:mm:ss');
  }

  // If it's already a string in YYYY-MM-DD format, append time
  if (typeof date === 'string') {
    // If it already has time component, return as-is
    if (date.includes('T')) {
      return date;
    }
    // Otherwise append midnight time
    return `${date}T00:00:00`;
  }

  // If it's a Dayjs object, format it
  return dayjs(date).format('YYYY-MM-DDTHH:mm:ss');
}

/**
 * Formats a UTC date string for display in the configured timezone
 * Converts UTC dates from backend to local timezone for display
 * 
 * @param dateString - The UTC date string from the backend (e.g., "2025-12-02T20:30:00" or "2025-12-02T20:30:00Z")
 * @param format - The desired output format (default: "MM-DD-YYYY")
 * @returns Formatted date string in the configured timezone
 * 
 * @example
 * Backend sends (UTC): "2025-12-02T20:30:00"
 * Timezone: America/Toronto (UTC-5)
 * formatDateForDisplay(date, "MM-DD-YYYY") => "12-02-2025"
 * formatDateForDisplay(date, "YYYY-MM-DD HH:mm") => "2025-12-02 15:30"
 */
export function formatDateForDisplay(dateString: string | null | undefined, format: string = 'MM-DD-YYYY'): string {
  if (!dateString) return '';

  try {
    // Get the configured timezone from environment
    const tz = ENV.timezone;

    // Parse the UTC date and convert to configured timezone
    const date = dayjs.utc(dateString).tz(tz);

    // Format based on requested format
    switch (format) {
      case 'MM-DD-YYYY':
        return date.format('MM-DD-YYYY');
      case 'YYYY-MM-DD':
        return date.format('YYYY-MM-DD');
      case 'DD-MM-YYYY':
        return date.format('DD-MM-YYYY');
      case 'YYYY-MM-DD HH:mm':
        return date.format('YYYY-MM-DD HH:mm');
      case 'MM-DD-YYYY HH:mm':
        return date.format('MM-DD-YYYY HH:mm');
      default:
        // Support any custom dayjs format string
        return date.format(format);
    }
  } catch (error) {
    console.error('Error formatting date:', error);
    return dateString;
  }
}

/**
 * Get the current configured timezone
 * @returns The IANA timezone string (e.g., "America/Toronto")
 */
export function getConfiguredTimezone(): string {
  return ENV.timezone;
}
