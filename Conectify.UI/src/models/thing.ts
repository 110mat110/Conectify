import { Metadata } from "./metadata";

export interface Thing {
    id: string;
    iPAdress: string;
    macAdress: string;
    thingName: string;
    positionId: string | null;
    position: Position;
    metadata: Metadata[];
}

export interface Position {
    description: string;
    lat: number;
    long: number;
}