export interface Metadata{
    id: string,
    name: string,
    numericValue: number,
    stringValue: string,
    unit: string;
    minVal: number,
    maxVal: number,
    metadataId: string
}

export interface ApiMetadata{
    name: string,
    id: string,
    exclusive: boolean
}

export interface ApiMetadataConnector{
    name: string,
    numericValue: number,
    stringValue: string,
    unit: string;
    minVal: number,
    maxVal: number,
    metadataId: string,
    deviceId: string,
    typeValue: number,
}