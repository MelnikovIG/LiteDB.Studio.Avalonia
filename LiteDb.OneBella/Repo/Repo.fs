module OneBella.Repo

open LiteDB
open LiteDb.Studio.Avalonia.Infra

let private db = new LiteDatabase(Utils.GetAppDataPath () + $"/{Utils.AppName}.db")

let getDb () = db

let disconnect () = db.Dispose()
