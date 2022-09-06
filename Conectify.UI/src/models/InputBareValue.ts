export interface InputBareType {
    type: string;
    destinationId: string | null;
    sourceId: string;
    timeCreated: string;
    name: string;
    stringValue: string;
    numericValue: number | null;
    unit: string;
}