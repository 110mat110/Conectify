import { AutomatizationBaseWithTarget } from "../automatizationComponent";

export class ValueInitRule extends AutomatizationBaseWithTarget{

    public sourceId: string = "";
    constructor(id: string){
        super(id)
    }
}