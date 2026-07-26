// Lean shapes returned by the UI service - exactly what a cube renders, nothing more.

export interface UiValue {
  name: string;
  unit: string;
  stringValue: string;
  numericValue: number | null;
  timeCreated: number;
}

export interface UiThreshold {
  min: number;
  max: number;
  color: string;
}

export interface UiIcon {
  name: string;
  stringValue: string;
}

// Chart series point: [timeCreated, numericValue]
export type UiPoint = [number, number | null];
