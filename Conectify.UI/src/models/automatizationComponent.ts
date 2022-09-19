export class AutomatizationBase{
    public id: string;

    public dragPosition = {x: 0, y: 0};

    public initialPos = {x:0, y:0};

    constructor(id: string){
        this.id = id;
    }
}


export class AutomatizationBaseWithTarget extends AutomatizationBase{

    public targets: string[] = [];

    constructor(id: string){
        super(id);
    }
}