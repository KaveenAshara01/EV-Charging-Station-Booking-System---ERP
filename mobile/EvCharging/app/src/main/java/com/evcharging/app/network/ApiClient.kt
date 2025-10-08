package com.evcharging.app.network

import com.evcharging.app.database.DatabaseHelper
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import android.content.Context

object ApiClient {

    private const val BASE_URL = "http://10.73.58.212:5074/api/"

    private lateinit var appContext: Context

    fun init(context: Context) {
        appContext = context.applicationContext
    }

    private val client by lazy {
        val logging = HttpLoggingInterceptor().apply { level = HttpLoggingInterceptor.Level.BODY }

        OkHttpClient.Builder()
            .addInterceptor(logging)
            .addInterceptor { chain ->
                val original = chain.request()
                val dbHelper = DatabaseHelper(appContext)
                val token = dbHelper.getUserSession()?.get("token")  // assuming token is stored here
                val requestBuilder = original.newBuilder()
                    .header("Content-Type", "application/json")
                if (!token.isNullOrEmpty()) {
                    requestBuilder.header("Authorization", "Bearer $token")
                }
                val request = requestBuilder.build()
                chain.proceed(request)
            }
            .build()
    }

    private val retrofit by lazy {
        Retrofit.Builder()
            .baseUrl(BASE_URL)
            .client(client)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }

    val retrofitService: ApiService by lazy {
        retrofit.create(ApiService::class.java)
    }
}
