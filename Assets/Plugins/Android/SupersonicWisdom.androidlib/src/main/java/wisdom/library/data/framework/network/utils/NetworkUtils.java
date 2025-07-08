package wisdom.library.data.framework.network.utils;

import android.content.Context;
import android.net.ConnectivityManager;
import android.net.Network;
import android.net.NetworkCapabilities;
import android.net.NetworkRequest;
import android.provider.Settings;

import androidx.annotation.NonNull;

import org.json.JSONObject;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;

import wisdom.library.api.listener.IWisdomConnectivityListener;
import wisdom.library.util.SdkLogger;
import wisdom.library.util.SwUtils;

public class NetworkUtils {
    public static final String KEY_IS_AVAILABLE = "isAvailable";
    public static final String KEY_IS_FLIGHT_MODE = "isFlightMode";
    private boolean isAvailable = false;
    private static final HashSet<Integer> mAvailableNetworks = new HashSet<>();
    private final Context _context;

    private final List<IWisdomConnectivityListener> mConnectivityListeners = Collections.synchronizedList(new ArrayList<>());

    private final ConnectivityManager.NetworkCallback networkCallback = new ConnectivityManager.NetworkCallback() {
        @Override
        public void onCapabilitiesChanged(@NonNull Network network, @NonNull NetworkCapabilities networkCapabilities) {
            synchronized (mAvailableNetworks) {
                validateIsAvailable(network, networkCapabilities);
            }
        }

        @Override
        public void onLost(@NonNull Network network) {
            synchronized (mAvailableNetworks) {
                handleNetworkLoss(network);
            }
        }
    };

    private final NetworkRequest networkRequest = new NetworkRequest.Builder()
        .addCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET)
        .build();

    public NetworkUtils(Context context) {
        this._context = context.getApplicationContext();
        registerToConnectivityManager();
    }

    private void registerToConnectivityManager() {
        ConnectivityManager connectivityManager = (ConnectivityManager) _context.getSystemService(Context.CONNECTIVITY_SERVICE);
        connectivityManager.registerNetworkCallback(networkRequest, networkCallback);
    }

    public boolean isFlightMode() {
        return Settings.System.getInt(_context.getContentResolver(),
            Settings.Global.AIRPLANE_MODE_ON, 0) != 0;
    }

    public synchronized boolean isNetworkAvailable() {
        return isAvailable();
    }

    public void registerToNetworkChanges(IWisdomConnectivityListener listener) {
        mConnectivityListeners.add(listener);
    }

    public void unregisterToNetworkChanges(IWisdomConnectivityListener listener) {
        mConnectivityListeners.remove(listener);
    }

    private void notifyConnectivityListeners() {
        SdkLogger.log("Network is available: " + isAvailable());
    
        JSONObject connection = new JSONObject();
        SwUtils.addToJson(connection, NetworkUtils.KEY_IS_AVAILABLE, isAvailable());
        SwUtils.addToJson(connection, NetworkUtils.KEY_IS_FLIGHT_MODE, isFlightMode());
        String connectionString = connection.toString();
    
        synchronized (mConnectivityListeners) {
            for (IWisdomConnectivityListener listener : mConnectivityListeners) {
                if (listener != null) {
                    listener.onConnectionStatusChanged(connectionString);
                }
            }
        }
    }


    private void validateIsAvailable(Network network, NetworkCapabilities networkCapabilities) {
        boolean hasInternet = networkCapabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET);
        boolean hasValidated = true;

        // This is a workaround for the issue that the networkCapabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_VALIDATED) is not working on Android 6.0
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.M) {
            hasValidated = networkCapabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_VALIDATED);
        }

        int networkHash = network.hashCode();

        if (hasInternet && hasValidated) {
            if (mAvailableNetworks.add(networkHash) && !isAvailable()) {
                setAvailable(true);
                notifyConnectivityListeners();
            }
            
            SdkLogger.log("Network available: " + network);
        } else {
            if (handleNetworkLoss(network)) {
                SdkLogger.log("Network lost due to capability loss: " + network);
            }
        }
    }

    private boolean handleNetworkLoss(@NonNull Network network) {
        boolean removed = mAvailableNetworks.remove(network.hashCode());
        SdkLogger.log("Network lost: " + network + " | removed: " + removed);

        if (mAvailableNetworks.isEmpty()) {
            setAvailable(false);
            notifyConnectivityListeners();
        }
        
        return removed;
    }

    private synchronized boolean isAvailable() {
        return isAvailable;
    }

    private synchronized void setAvailable(boolean available) {
        isAvailable = available;
    }
}