export interface Multiresult<T>{
    exceptions : Exception[],
    entities: T[],
    NoResults: number
}

export interface Exception{
    Message: string,
}

export interface SingleResult<T>{
    entity: T,
    exceptions : Exception[],
}