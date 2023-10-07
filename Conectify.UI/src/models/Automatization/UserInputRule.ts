import { AutomatizationBaseGeneric } from "../automatizationComponent";

export class UserInputRule extends AutomatizationBaseGeneric<UserInputBehaviour>{}

interface UserInputBehaviour{
    SourceActuatorId : string;
    Name: string;
}