package com.evcharging.app.database

import android.content.ContentValues
import android.content.Context
import android.database.Cursor
import android.database.sqlite.SQLiteDatabase
import android.database.sqlite.SQLiteOpenHelper

class DatabaseHelper(context: Context) :
    SQLiteOpenHelper(context, DATABASE_NAME, null, DATABASE_VERSION) {

    companion object {
        private const val DATABASE_NAME = "ev_app.db"
        private const val DATABASE_VERSION = 1
        private const val TABLE_USER_SESSION = "user_session"

        private const val COLUMN_ID = "id"
        private const val COLUMN_NAME = "name"
        private const val COLUMN_EMAIL = "email"
        private const val COLUMN_ROLE = "role"
        private const val COLUMN_TOKEN = "token"
    }

    override fun onCreate(db: SQLiteDatabase) {
        val createTable = """
            CREATE TABLE $TABLE_USER_SESSION (
                $COLUMN_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                $COLUMN_NAME TEXT,
                $COLUMN_EMAIL TEXT,
                $COLUMN_ROLE TEXT,
                $COLUMN_TOKEN TEXT
            )
        """.trimIndent()
        db.execSQL(createTable)
    }

    override fun onUpgrade(db: SQLiteDatabase, oldVersion: Int, newVersion: Int) {
        db.execSQL("DROP TABLE IF EXISTS $TABLE_USER_SESSION")
        onCreate(db)
    }

    fun saveUserSession(name: String, email: String, role: String, token: String) {
        val db = writableDatabase
        db.delete(TABLE_USER_SESSION, null, null) // clear old sessions
        val values = ContentValues().apply {
            put(COLUMN_NAME, name)
            put(COLUMN_EMAIL, email)
            put(COLUMN_ROLE, role)
            put(COLUMN_TOKEN, token)
        }
        db.insert(TABLE_USER_SESSION, null, values)
        db.close()
    }

    fun getUserSession(): Map<String, String>? {
        val db = readableDatabase
        val cursor: Cursor =
            db.rawQuery("SELECT * FROM $TABLE_USER_SESSION LIMIT 1", null)

        var session: Map<String, String>? = null
        if (cursor.moveToFirst()) {
            session = mapOf(
                "name" to cursor.getString(cursor.getColumnIndexOrThrow(COLUMN_NAME)),
                "email" to cursor.getString(cursor.getColumnIndexOrThrow(COLUMN_EMAIL)),
                "role" to cursor.getString(cursor.getColumnIndexOrThrow(COLUMN_ROLE)),
                "token" to cursor.getString(cursor.getColumnIndexOrThrow(COLUMN_TOKEN))
            )
        }
        cursor.close()
        db.close()
        return session
    }

    fun clearSession() {
        val db = writableDatabase
        db.delete(TABLE_USER_SESSION, null, null)
        db.close()
    }
}
