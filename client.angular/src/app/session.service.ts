import { Injectable } from '@angular/core';
import { generateId } from '/idGen';

const charPool = '0123456789abcdef';

@Injectable({
  providedIn: 'root'
})
export class SessionService {

  get clientId() {
    const key = 'clientId';
    let id = sessionStorage.getItem(key);
    if (!id) {
      id = generateId(6, charPool);
      sessionStorage.setItem(key, id);
    }

    return id;
  }
}
