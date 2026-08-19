/** A group of rules with its own inputs and outputs. Functions can hold other functions. */
export interface FunctionModel {
    id: string;
    name: string;
    description: string;
    parentFunctionId?: string;
    x: number;
    y: number;
    inputPorts: FunctionPort[];
    outputPorts: FunctionPort[];
}

/**
 * One pin of a function. `pointId` is what a connection is made to - the input point of the rule
 * behind an input port, the output point of the rule behind an output port.
 */
export interface FunctionPort {
    ruleId: string;
    pointId: string;
    name: string;
    type: number;
    index: number;
}

export interface CreateFunction {
    name: string;
    parentFunctionId?: string;
}

export interface EditFunction {
    name: string;
    description: string;
    x: number;
    y: number;
}

export enum FunctionPortDirection {
    Input = 0,
    Output = 1,
}

export interface AddFunctionPort {
    direction: FunctionPortDirection;
    type: number;
    name: string;
}

export interface MoveToFunction {
    functionId?: string;
}
