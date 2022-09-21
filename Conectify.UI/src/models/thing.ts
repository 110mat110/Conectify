export interface Device {
    id: string;
    iPAdress: string;
    macAdress: string;
    name: string;
    positionId: string | null;
    position: Position | null;
}

export interface Position {
    description: string;
    lat: number;
    long: number;
}

export interface DeviceSelector{
    id: string;
    name: string;
    type: number;
}