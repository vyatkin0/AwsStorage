import { Component, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

/**
 * AwsStorage frontend
 */

@Component({
  selector: 'app-root',
  templateUrl: './storage.component.html',
})
export class StorageComponent {

  public itemKey: string;
  public itemValue: string;
  public getItemKey: string;
  public itemPrefix: string;

  public errorMessage: string;
  public lastResult: string;
  public getErrorMessage: string;
  public getLastResult: string;
  public listErrorMessage: string;
  public listLastResult: string;

  private _http: HttpClient;
  private _baseUrl: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this._http = http;
    this._baseUrl = baseUrl;
  }

  /**
   * Sets an item
   */
  public setItem() {
    this.lastResult = null;
    this.errorMessage = null;
    try {
      const httpOptions: Object = {
        responseType: 'text'
      };

      const model: SetItemModel = { key: this.itemKey, value: this.itemValue};

      this._http.post<string>(this._baseUrl + 'api/Storage/SetItem', model, httpOptions).subscribe(result => {
        this.lastResult = result;
      }, error => this.errorMessage = JSON.stringify(error));
    } catch (e) {
      this.errorMessage = e;
    }
  }

  /**
   * Gets or delete an item
   * @param del - if true then delete item else get item.
   */
  public getItem(del: boolean) {
    this.getLastResult = null;
    this.getErrorMessage = null;
    try {
      const httpOptions: Object = {
        headers: new HttpHeaders({
          'Content-Type':  'application/json',
        }),
        responseType: 'text'
      };

      const url: string = del ? 'api/Storage/RemoveItem' : 'api/Storage/GetItem';
      this._http.post<string>(this._baseUrl + url, `"${this.getItemKey}"`, httpOptions).subscribe(result => {
        this.getLastResult = result;
      }, error => this.getErrorMessage = JSON.stringify(error));
    } catch (e) {
      this.getErrorMessage = e;
    }
  }

  /**
   * Lists items
   */
  public listItems() {
    this.listLastResult = null;
    this.listErrorMessage = null;
    try {
      const model: ListItemsModel = { prefix: this.itemPrefix, limit: 100};
      this._http.post(this._baseUrl + 'api/Storage/List', model).subscribe(result => {
        this.listLastResult = JSON.stringify(result);
      }, error => this.listErrorMessage = JSON.stringify(error));
    } catch (e) {
      this.listErrorMessage = e;
    }
  }

  /**
   * Removes all items
   */
  public clearItems() {
    this.listLastResult = null;
    this.listErrorMessage = null;
    try {
      const httpOptions: Object = {
        headers: new HttpHeaders({
          'Content-Type':  'application/json',
        }),
        responseType: 'text'
      };

      this._http.post<string>(this._baseUrl + 'api/Storage/Clear', `"${this.itemPrefix}"`, httpOptions).subscribe(result => {
        this.listLastResult = result;
      }, error => this.listErrorMessage = JSON.stringify(error));
    } catch (e) {
      this.listErrorMessage = e;
    }
  }
}

interface SetItemModel {
  key: string;
  value: string;
}

interface ListItemsModel {
  prefix: string;
  limit: number;
}

