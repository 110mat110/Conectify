import { Injectable } from '@angular/core';
import MetadataMapper from '../assets/icons/metadataMapper.json'
import { MessagesService } from './messages.service';

@Injectable({
  providedIn: 'root'
})
export class MetadataIconService {

  constructor(private msg: MessagesService) { }

  GetIconAdressForMetadata(name: string, strValue: string): string | undefined{
    this.msg.addMessage("requesing pic for: " + name + " " + strValue);
    return MetadataMapper.find(x => x.name === name && x.stringValue === strValue)?.pic;
  }
}
