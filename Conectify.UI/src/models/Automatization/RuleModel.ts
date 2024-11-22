import { input, output } from "@angular/core";
import { InputApiModel, OutputApiModel } from "../automatizationComponent";

export interface RuleModel{
    id: string;
    x: number;
    y: number;
    propertyJson: string;
    behaviourId: string;
    name: string;
    outputs: OutputApiModel[];
    inputs: InputApiModel[];
}

export interface AddInputApiModel{
    ruleId: string;
    inputType: number;
    index: number;
}

export interface AddOutputApiModel{
    ruleId: string;
    index: number;
}