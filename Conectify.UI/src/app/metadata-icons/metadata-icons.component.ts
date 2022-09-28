import { Component, Input, OnChanges, OnInit } from '@angular/core';
import { Metadata } from 'src/models/metadata';
import { MessagesService } from '../messages.service';
import { MetadataIconService } from '../metadata-icon.service';

@Component({
  selector: 'app-metadata-icons',
  templateUrl: './metadata-icons.component.html',
  styleUrls: ['./metadata-icons.component.css']
})
export class MetadataIconsComponent implements OnChanges {

  @Input() metadatas: Metadata[] = [];
  pics: string[] = [];
  constructor(private msg: MessagesService ,private mis: MetadataIconService) { }

  ngOnChanges(): void{
    this.ShowAllIcons();
  }

  ShowAllIcons(){
    this.pics = [];
    this.metadatas.slice().forEach(
      x =>{
      var pic = this.mis.GetIconAdressForMetadata(x.name, x.stringValue)
      if(pic){
        this.msg.addMessage(pic);
        this.pics.push(pic);
      }
      }
    )
  }
}
