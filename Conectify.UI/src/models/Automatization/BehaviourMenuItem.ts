export interface BehaviourMenuItem{
    id: string;
    name: string;
    outputs: MinMaxDef,
    inputs: Tuple[]
  }
  
  export class MinMaxDef {
    constructor(
      public min: number,
      public def: number,
      public max: number
    ) {}
  }

  export class Tuple {
    constructor(
        public item1: number,
        public item2: MinMaxDef
    ) {}
  }