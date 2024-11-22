export class AutomatizationBase{
    public id: string;

    public dragPosition = {x: 0, y: 0};

    public drawingPos = {x: 0, y: 0};
    public behaviorId: string = "";

    public inputs: InputApiModel[] = [];
    public outputs: OutputApiModel[] = [];

    public triggerOnValue: boolean = false;
    public name: string = "";

    public getParametersJSon(): string {
        return "{}";
    };

    constructor(id: string, behaviourId: string, name: string){
        this.id = id;
        this.behaviorId = behaviourId;
        this.name = name;
    }
}

export interface InputApiModel{
    id: string;
    index: number;
    type: number; 
}

export interface OutputApiModel{
    id: string;
    index: number;
}

export class AutomatizationBaseGeneric<T> extends AutomatizationBase{
    public behaviour: T;

    public getParametersJSon(): string {
        return JSON.stringify(this.behaviour);
    };

    constructor(id: string, behaviourId: string, json: string, defaultT: T, name:string){
    super(id, behaviourId, name)
        this.behaviour = defaultT;
        try {
            let behaviour: T = JSON.parse(json) as T;
            if (behaviour) {
                this.behaviour = behaviour;
            }
        }
        catch (e) {
            console.error(e);
        }
    }
}