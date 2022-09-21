export interface Metadata{
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