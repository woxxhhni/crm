import type { ThemeConfig } from "antd";
import { theme } from "antd";

/* ─────────────────────────────────────────────
 * CLS Design Tokens — Light Mode Only
 * ───────────────────────────────────────────── */

export const colors = {
  // Brand
  primary:    "#1B3A5C",
  accent:     "#3B82F6",

  // Semantic
  success:    "#10B981",
  warning:    "#F59E0B",
  danger:     "#EF4444",

  // Surfaces
  bg:         "#F8FAFC",
  surface:    "#FFFFFF",
  text:       "#0F172A",
  textMuted:  "#64748B",
  border:     "#E2E8F0",

  // Sidebar (always dark navy)
  sidebar:    "#0F1729",

  // Status
  status: {
    completed:  "#10B981",
    inProgress: "#3B82F6",
    suspended:  "#F59E0B",
    canceled:   "#EF4444",
  },
} as const;

/** Ant Design theme configuration — light mode */
export const themeConfig: ThemeConfig = {
  algorithm: theme.defaultAlgorithm,
  token: {
    colorPrimary:       colors.accent,
    colorInfo:          colors.accent,
    colorSuccess:       colors.success,
    colorWarning:       colors.warning,
    colorError:         colors.danger,
    colorBgLayout:      colors.bg,
    colorBgContainer:   colors.surface,
    colorText:          colors.text,
    colorTextSecondary: colors.textMuted,
    colorBorder:        colors.border,
    borderRadius:       10,
    fontFamily:         "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
    fontSize:           14,
    controlHeight:      40,
  },
  components: {
    Button: {
      borderRadius: 10,
      controlHeight: 40,
      paddingInline: 20,
    },
    Card: {
      borderRadiusLG: 16,
    },
    Modal: {
      borderRadiusLG: 16,
    },
    Input: {
      borderRadius: 10,
      controlHeight: 42,
    },
    Select: {
      borderRadius: 10,
      controlHeight: 42,
    },
    Table: {
      borderRadiusLG: 12,
      headerBg: colors.bg,
      headerColor: colors.textMuted,
      headerSplitColor: colors.border,
      rowHoverBg: '#F1F5F9',
      cellPaddingBlock: 14,
      cellPaddingInline: 16,
    },
    Pagination: {
      itemActiveBg: '#EFF6FF',
      itemBg: 'transparent',
      itemSize: 36,
      borderRadius: 8,
      colorPrimary: colors.primary,
      colorPrimaryHover: colors.accent,
      fontSize: 14,
      controlHeight: 36,
    },
    Menu: {
      itemBorderRadius: 8,
      itemMarginInline: 8,
    },
  },
};

export default themeConfig;
